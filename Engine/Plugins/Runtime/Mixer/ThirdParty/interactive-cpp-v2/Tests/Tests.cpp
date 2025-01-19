#include "stdafx.h"
#include "CppUnitTest.h"
#include <interactivity.h>
#include <iostream>
#include <fstream>
#include <thread>
#include <iomanip>      // std::setprecision
#include <condition_variable>
#include <map>
#if _WIN32
#include <Windows.h>
#endif

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

#define ASSERT_NOERR(x) do {err = x; Assert::IsTrue(0 == err); if (err) return;} while (0)
#define ASSERT_RETERR(x) do {err = x; Assert::IsTrue(0 == err); if (err) return err;} while (0)

namespace MixerTests
{

#define CLIENT_ID "f0d20e2d263b75894f5cdaabc8a344b99b1ea6f9ecb7fa4f"
#define VERSION_ID "135704"
#define SHARE_CODE "xe7dpqd5"

#define DO_NOT_APPROVE_CLIENT_ID "b33e730969b2f1234afd94dff745dd1d2c4e7557e168ebce"
#define DO_NOT_APPROVE_CLIENT_SECRET "98d47b58be9917e68769a9b687c8b68fb24daeed3d6a11aa5cfb65df44c54e59"

interactive_session g_activeSession = nullptr;

template <typename T>
struct async_context
{
	async_context() : taskComplete(false), taskData(nullptr) {};
	std::mutex taskMutex;
	std::condition_variable taskCV;
	volatile bool taskComplete;
	const T* taskData;
};

void print_control_properties(interactive_session session, const std::string& controlId)
{
	char propName[1024];
	size_t propLength = sizeof(propName);
	interactive_property_type propType = interactive_unknown_t;
	size_t propCount = 0;
	int err = interactive_control_get_property_count(session, controlId.c_str(), &propCount);
	std::stringstream s;
	s << "[ControlData] ";
	for (size_t i = 0; i < propCount && err == MIXER_OK; ++i)
	{
		err = interactive_control_get_property_data(session, controlId.c_str(), i, propName, &propLength, &propType);
		if (MIXER_OK == err)
		{
			switch (propType)
			{
			case interactive_int_t:
			{
				int propVal;
				err = interactive_control_get_property_int(session, controlId.c_str(), propName, &propVal);
				if (MIXER_OK == err)
				{
					s << propName << ": '" << propVal << "' ";
				}
				break;
			}
			case interactive_bool_t:
			{
				bool propVal;
				err = interactive_control_get_property_bool(session, controlId.c_str(), propName, &propVal);
				if (MIXER_OK == err)
				{
					s << propName << ": '" << propVal << "' ";
				}
				break;
			}
			case interactive_float_t:
			{
				float propVal;
				err = interactive_control_get_property_float(session, controlId.c_str(), propName, &propVal);
				if (MIXER_OK == err)
				{
					s << propName << ": '" << propVal << "' ";
				}
				break;
			}
			case interactive_string_t:
			{
				char* propVal = nullptr;
				size_t propValLength = 0;
				err = interactive_control_get_property_string(session, controlId.c_str(), propName, propVal, &propValLength);
				if (err == MIXER_ERROR_BUFFER_SIZE && propValLength > 0)
				{
					propVal = new char[propValLength];
					err = interactive_control_get_property_string(session, controlId.c_str(), propName, propVal, &propValLength);
					if (MIXER_OK == err)
					{
						s << propName << ": '" << propVal << "' ";
					}
					delete[] propVal;
				}
				break;
			}
			case interactive_array_t:
			{
				s << propName << ": <Array> ";
				break;
			}
			case interactive_object_t:
			{
				s << propName << ": <Object> ";
				if (0 != strcmp("meta", propName))
				{
					break;
				}

				// Get metadata properties.
				size_t metapropCount = 0;
				err = interactive_control_get_meta_property_count(session, controlId.c_str(), &metapropCount);
				for (size_t i = 0; i < metapropCount && MIXER_OK == err; ++i)
				{
					char* metapropName = nullptr;
					size_t metapropNameLength = 0;
					interactive_property_type metapropType = interactive_unknown_t;
					err = interactive_control_get_meta_property_data(session, controlId.c_str(), i, metapropName, &metapropNameLength, &metapropType);
					if (err == MIXER_ERROR_BUFFER_SIZE && metapropNameLength > 0)
					{
						metapropName = new char[metapropNameLength];
						err = interactive_control_get_meta_property_data(session, controlId.c_str(), i, metapropName, &metapropNameLength, &metapropType);
						if (err) break;
						switch (metapropType)
						{
						case interactive_int_t:
						{
							int metaVal;
							err = interactive_control_get_meta_property_int(session, controlId.c_str(), metapropName, &metaVal);
							if (err) break;
							s << "meta\\" << metapropName << ": '" << metaVal << "' ";
							break;
						}
						case interactive_bool_t:
						{
							bool metaVal;
							err = interactive_control_get_meta_property_bool(session, controlId.c_str(), metapropName, &metaVal);
							if (err) break;
							s << "meta\\" << metapropName << ": '" << metaVal << "' ";
							break;
						}
						case interactive_float_t:
						{
							float metaVal;
							err = interactive_control_get_meta_property_float(session, controlId.c_str(), metapropName, &metaVal);
							if (err) break;
							s << "meta\\" << metapropName << ": '" << metaVal << "' ";
							break;
						}
						case interactive_string_t:
						{
							char metaVal[4096];
							size_t metaValLength = sizeof(metaVal);
							err = interactive_control_get_meta_property_string(session, controlId.c_str(), metapropName, metaVal, &metaValLength);
							if (err) break;
							s << "meta\\" << metapropName << ": '" << metaVal << "' ";
							break;
						}
						case interactive_array_t:
						{
							s << "meta\\" << metapropName << ": <Array> ";
							Logger::WriteMessage((std::string("meta\\") + std::string(metapropName) + ": <Array>").c_str());
							break;
						}
						case interactive_unknown_t:
						default:
							s << "meta\\" << metapropName << ": <Unknown> ";
							break;
						}
						delete[] metapropName;
					}
				}
				break;
			}
			case interactive_unknown_t:
			default:
				s << propName << ": <Unknown> ";
				break;
			}
		}
	}

	Logger::WriteMessage(s.str().c_str());
}

void print_participant_properties(interactive_session session, const std::string& participantId)
{
	int err = 0;
	unsigned long long connectedAt = 0;
	unsigned long long lastInputAt = 0;
	char* group = nullptr;
	size_t groupLength = 0;
	char* userName = nullptr;
	size_t userNameLength = 0;
	unsigned int userId = 0;
	bool isDisabled = false;
	unsigned int level = 0;

	ASSERT_NOERR(interactive_get_participant_connected_at(session, participantId.c_str(), &connectedAt));
	err = interactive_get_participant_group(session, participantId.c_str(), group, &groupLength);
	if (MIXER_ERROR_BUFFER_SIZE == err)
	{
		group = new char[groupLength];
		ASSERT_NOERR(interactive_get_participant_group(session, participantId.c_str(), group, &groupLength));
	}
	Assert::IsTrue(MIXER_OK == err);
	ASSERT_NOERR(interactive_get_participant_is_disabled(session, participantId.c_str(), &isDisabled));
	ASSERT_NOERR(interactive_get_participant_last_input_at(session, participantId.c_str(), &lastInputAt));
	ASSERT_NOERR(interactive_get_participant_level(session, participantId.c_str(), &level));
	ASSERT_NOERR(interactive_get_participant_user_id(session, participantId.c_str(), &userId));
	err = interactive_get_participant_user_name(session, participantId.c_str(), userName, &userNameLength);
	if (MIXER_ERROR_BUFFER_SIZE == err)
	{
		userName = new char[userNameLength];
		ASSERT_NOERR(interactive_get_participant_user_name(session, participantId.c_str(), userName, &userNameLength));
	}
	Assert::IsTrue(MIXER_OK == err);

	std::stringstream s;
	s << "[Participant] '" << userName << "' (" << level << ") Enabled: " << std::to_string(!isDisabled) << " Group: '" << group << "' Connected At: " << connectedAt << " Last Input At: " << lastInputAt;
	Logger::WriteMessage(s.str().c_str());

	delete[] group;
	delete[] userName;
}

struct transaction_data
{
	interactive_input_type type;
	std::string participantName;
	std::string controlId;
};

static std::map<std::string, transaction_data> g_dataByTransaction;

void handle_transaction_complete(void* context, interactive_session session, const char* transactionId, size_t transactionIdLength, unsigned int errorCode, const char* errorMessage, size_t errorMessageSize)
{
	if (errorCode)
	{
		std::string message = std::string("Failed to capture transaction '") + transactionId + "', " + errorMessage + " ( " + std::to_string(errorCode) + ")";
		Logger::WriteMessage(message.c_str());

		// Remove any stored transaction data.
		g_dataByTransaction.erase(std::string(transactionId, transactionIdLength));
		return;
	}

	auto transactionItr = g_dataByTransaction.find(transactionId);
	if (transactionItr != g_dataByTransaction.end())
	{
		transaction_data data = transactionItr->second;
		switch (data.type)
		{
		case input_type_click:
		case input_type_key:
			Logger::WriteMessage((std::string("Executing button press for participant: ") + data.participantName).c_str());

			// Look for a cooldown metadata property and tigger it if it exists.
			long long cooldown = 0;
			int err = interactive_control_get_meta_property_int64(session, data.controlId.c_str(), BUTTON_PROP_COOLDOWN, &cooldown);
			Assert::IsTrue(MIXER_OK == err || MIXER_ERROR_PROPERTY_NOT_FOUND == err);
			if (MIXER_OK == err && cooldown > 0)
			{
				ASSERT_NOERR(interactive_control_trigger_cooldown(session, data.controlId.c_str(), cooldown * 1000));
			}
			break;
		}

		// This data is no longer necessary, clean it up.
		g_dataByTransaction.erase(transactionItr);
	}
	else
	{
		Logger::WriteMessage("Transaction captured but found no associated transaction data.");
	}
}

void handle_input(void* context, interactive_session session, const interactive_input* input)
{
	int err = 0;

	Logger::WriteMessage((std::string("Input detected, raw JSON: ") + std::string(input->jsonData, input->jsonDataLength)).c_str());

	switch (input->type)
	{
	case input_type_click:
	case input_type_key:
	{
		if (input->buttonData.action == interactive_button_action_down)
		{
			// This requries a transaction. Store relevant data.
			transaction_data data;
			memset(&data, 0, sizeof(transaction_data));
			data.type = input->type;
			size_t nameLength = 0;
			int err = interactive_get_participant_user_name(session, input->participantId, nullptr, &nameLength);
			Assert::IsTrue(MIXER_ERROR_BUFFER_SIZE == err);
			data.participantName.resize(nameLength);
			ASSERT_NOERR(interactive_get_participant_user_name(session, input->participantId, (char*)data.participantName.data(), &nameLength));
			data.controlId = std::string(input->control.id, input->control.idLength);
			g_dataByTransaction[std::string(input->transactionId, input->transactionIdLength)] = data;
		}
		else
		{
			Logger::WriteMessage("KEYUP");
		}
		break;
	}
	case input_type_move:
	{
		Logger::WriteMessage((std::string("MOVEMENT on ") + input->control.id).c_str());
		Logger::WriteMessage(("X:\t" + std::to_string(input->coordinateData.x)).c_str());
		Logger::WriteMessage(("Y:\t" + std::to_string(input->coordinateData.y)).c_str());

		break;
	}
	case input_type_custom:
	default:
	{
		Logger::WriteMessage("CUSTOM input.");
		break;
	}
	}

	// Print transaction id if there is one.
	if (nullptr != input->transactionId)
	{
		std::string s = std::string("Capturing transaction: ") + input->transactionId;
		Logger::WriteMessage(s.c_str());
		// Capture the transaction asynchronously.
		ASSERT_NOERR(interactive_capture_transaction(session, input->transactionId));
	}

	print_control_properties(session, input->control.id);
	if (0 != input->participantIdLength)
	{
		print_participant_properties(session, std::string(input->participantId, input->participantIdLength));
	}
}

void handle_state_changed(void* context, interactive_session session, interactive_state previousState, interactive_state newState)
{
	switch (newState)
	{
	case interactive_state::disconnected:
	{
		Logger::WriteMessage("Interactive disconnected");
		break;
	}
	case interactive_state::not_ready:
	{
		g_activeSession = session;
		Logger::WriteMessage("Interactive connected - waiting.");
		break;
	}
	case interactive_state::ready:
	{
		Logger::WriteMessage("Interactive connected - ready.");
		break;
	}
	default:
	{
		Logger::WriteMessage("ERROR: Unknown interactive state.");
		break;
	}
	}
}

void handle_participants_changed(void* context, interactive_session session, interactive_participant_action action, const interactive_participant* participant)
{
	std::stringstream s;
	std::string actionStr;
	switch (action)
	{
	case participant_join:
		actionStr = "joined";
		break;
	case participant_leave:
		actionStr = "left";
		break;
	case participant_update:
	default:
		actionStr = "updated";
		break;
	}
	s << "'" << participant->userName << "' (" << participant->level << ") " << actionStr << ".\r\n";
	Logger::WriteMessage(s.str().c_str());
}

void handle_error(void* context, interactive_session session, int errorCode, const char* errorMessage, size_t errorMessageLength)
{
	Logger::WriteMessage(("[ERROR] (" + std::to_string(errorCode) + ")" + errorMessage).c_str());
}

void handle_unhandled_method(void* context, interactive_session session, const char* methodJson, size_t methodJsonLength)
{
	Logger::WriteMessage(("Unhandled method: " + std::string(methodJson, methodJsonLength)).c_str());
}

int do_short_code_auth(const std::string& clientId, const std::string& clientSecret, std::string& refreshToken)
{
	int err = 0;
	char shortCode[7];
	size_t shortCodeLength = sizeof(shortCode);
	char shortCodeHandle[1024];
	size_t shortCodeHandleLength = sizeof(shortCodeHandle);

	// Get an OAuth short code to display to the user.
	ASSERT_RETERR(interactive_auth_get_short_code(clientId.c_str(), clientSecret.c_str(), shortCode, &shortCodeLength, shortCodeHandle, &shortCodeHandleLength));
	
	std::string authUrl = std::string("https://www.mixer.com/go?code=") + shortCode;
	ShellExecuteA(0, 0, authUrl.c_str(), nullptr, nullptr, SW_SHOW);

	// Wait for OAuth token response.
	char refreshTokenBuffer[1024];
	size_t refreshTokenLength = sizeof(refreshTokenBuffer);
	ASSERT_RETERR(interactive_auth_wait_short_code(clientId.c_str(), clientSecret.c_str(), shortCodeHandle, refreshTokenBuffer, &refreshTokenLength));

	refreshToken = std::string(refreshTokenBuffer, refreshTokenLength);
	return 0;
}

int do_auth(const std::string& clientId, const std::string& clientSecret, std::string& auth)
{
	int err = 0;
	// Attempt to read refresh info if it exists.
	std::fstream refreshTokenFile;
	refreshTokenFile.open("refreshtoken", std::ios::in);
	std::stringstream ssRefreshToken;
	ssRefreshToken << refreshTokenFile.rdbuf();
	refreshTokenFile.close();
	std::string refreshToken = ssRefreshToken.str();
	if (refreshToken.empty())
	{
		ASSERT_RETERR(do_short_code_auth(clientId, clientSecret, refreshToken));

		std::ofstream refreshTokenFile;
		refreshTokenFile.open("refreshtoken", std::ios::trunc);
		refreshTokenFile << refreshToken;
		refreshTokenFile.close();

		Logger::WriteMessage("New token saved to refreshtoken file.");
	}
	else
	{
		// Check if the token is stale and needs to be refreshed.
		bool isStale = false;
		err = interactive_auth_is_token_stale(refreshToken.c_str(), &isStale);
		if (err || isStale)
		{
			if (isStale)
			{
				char tokenBuffer[1024];
				size_t tokenBufferLength = sizeof(tokenBuffer);
				err = interactive_auth_refresh_token(clientId.c_str(), clientSecret.c_str(), refreshToken.c_str(), tokenBuffer, &tokenBufferLength);
				if (!err)
				{
					refreshToken = std::string(tokenBuffer, tokenBufferLength);
				}
			}

			if (err)
			{
				ASSERT_RETERR(do_short_code_auth(clientId, clientSecret, refreshToken));
			}

			// Cache the refresh token
			refreshTokenFile.open("refreshtoken", std::ios::out | std::ios::trunc);
			refreshTokenFile << refreshToken;
			refreshTokenFile.flush();
			refreshTokenFile.close();
			Logger::WriteMessage("New token saved to refreshtoken file.");
		}
	}

	// Extract the authorization header from the refresh token.
	char authBuffer[1024];
	size_t authBufferLength = sizeof(authBuffer);
	ASSERT_RETERR(interactive_auth_parse_refresh_token(refreshToken.c_str(), authBuffer, &authBufferLength));
	auth = std::string(authBuffer, authBufferLength);
	return err;
}

static std::chrono::time_point<std::chrono::steady_clock> g_start;

void handle_debug_message(interactive_debug_level level, const char* dbgMsg, size_t dbgMsgSize)
{
	std::stringstream s;
	std::chrono::duration<double> elapsed = std::chrono::high_resolution_clock::now() - g_start;
	std::string type;
	switch (level)
	{
	case interactive_debug_error:
		type = "ERROR";
		break;
	case interactive_debug_trace:
		type = "TRACE ";
		break;
	case interactive_debug_warning:
		type = "WARN  ";
		break;
	case interactive_debug_info:
		type = "INFO  ";
		break;
	default:
		type = "DEBUG";
		break;
	}
	s << std::fixed << std::setprecision(4) << elapsed.count() << " [" << type << "] " << dbgMsg;
	Logger::WriteMessage(s.str().c_str());
}

TEST_CLASS(Tests)
{
public:
	TEST_METHOD(ConnectTest)
	{
		g_start = std::chrono::high_resolution_clock::now();
		interactive_config_debug(interactive_debug_trace, handle_debug_message);

		int err = 0;
		std::string clientId = CLIENT_ID;
		std::string versionId = VERSION_ID;
		std::string shareCode = SHARE_CODE;
		std::string auth;

		ASSERT_NOERR(do_auth(clientId, "", auth));

		interactive_session session;
		Logger::WriteMessage("Connecting...");
		ASSERT_NOERR(interactive_open_session(auth.c_str(), versionId.c_str(), shareCode.c_str(), true, &session));

		// Simulate 60 frames/sec for 1 seconds.
		const int fps = 60;
		const int seconds = 1;
		for (int i = 0; i < fps * seconds; ++i)
		{
			ASSERT_NOERR(interactive_run(session, 1));
			std::this_thread::sleep_for(std::chrono::milliseconds(1000 / fps));
		}

		Logger::WriteMessage("Disconnecting...");
		interactive_close_session(session);

		Assert::IsTrue(0 == err);
	}

	TEST_METHOD(ConnectWithSecretTest)
	{
		g_start = std::chrono::high_resolution_clock::now();
		interactive_config_debug(interactive_debug_trace, handle_debug_message);

		int err = 0;
		std::string clientId = DO_NOT_APPROVE_CLIENT_ID;
		std::string clientSecret = DO_NOT_APPROVE_CLIENT_SECRET;
		std::string versionId = VERSION_ID;
		std::string shareCode = SHARE_CODE;
		std::string auth;

		ASSERT_NOERR(do_auth(clientId, clientSecret, auth));

		interactive_session session;
		Logger::WriteMessage("Connecting...");
		ASSERT_NOERR(interactive_open_session(auth.c_str(), versionId.c_str(), shareCode.c_str(), true, &session));

		// Simulate 60 frames/sec for 1 seconds.
		const int fps = 60;
		const int seconds = 1;
		for (int i = 0; i < fps * seconds; ++i)
		{
			ASSERT_NOERR(interactive_run(session, 1));
			std::this_thread::sleep_for(std::chrono::milliseconds(1000 / fps));
		}

		Logger::WriteMessage("Disconnecting...");
		interactive_close_session(session);

		Assert::IsTrue(0 == err);
	}

	TEST_METHOD(InputTest)
	{
		g_start = std::chrono::high_resolution_clock::now();
		interactive_config_debug(interactive_debug_trace, handle_debug_message);

		int err = 0;
		std::string clientId = CLIENT_ID;
		std::string versionId = VERSION_ID;
		std::string shareCode = SHARE_CODE;
		std::string auth;

		ASSERT_NOERR(do_auth(clientId, "", auth));

		interactive_session session;
		ASSERT_NOERR(interactive_open_session(auth.c_str(), versionId.c_str(), shareCode.c_str(), true, &session));

		ASSERT_NOERR(interactive_register_input_handler(session, handle_input));
		ASSERT_NOERR(interactive_register_state_changed_handler(session, handle_state_changed));
		ASSERT_NOERR(interactive_register_error_handler(session, handle_error));
		ASSERT_NOERR(interactive_register_participants_changed_handler(session, handle_participants_changed));
		ASSERT_NOERR(interactive_register_unhandled_method_handler(session, handle_unhandled_method));
		ASSERT_NOERR(interactive_register_transaction_complete_handler(session, handle_transaction_complete));

		ASSERT_NOERR(interactive_set_bandwidth_throttle(session, throttle_participant_leave, 0, 0));
		ASSERT_NOERR(interactive_set_bandwidth_throttle(session, throttle_input, 2 * 1024 * 1024 /* 2mb */, 512 * 1024 /* 512kbps */));

		// Simulate 60 frames/sec
		const int fps = 60;
		const int seconds = 15;
		for (int i = 0; i < fps * seconds; ++i)
		{
			ASSERT_NOERR(interactive_run(session, 1));
			std::this_thread::sleep_for(std::chrono::milliseconds(1000 / fps));
		}

		Logger::WriteMessage("Disconnecting...");
		interactive_close_session(session);

		Assert::IsTrue(0 == err);
	}

	TEST_METHOD(ManualReadyTest)
	{
		g_start = std::chrono::high_resolution_clock::now();
		interactive_config_debug(interactive_debug_trace, handle_debug_message);

		int err = 0;
		std::string clientId = CLIENT_ID;
		std::string versionId = VERSION_ID;
		std::string shareCode = SHARE_CODE;
		std::string auth;

		ASSERT_NOERR(do_auth(clientId, "", auth));

		interactive_session session;
		ASSERT_NOERR(interactive_open_session(auth.c_str(), versionId.c_str(), shareCode.c_str(), false, &session));
		interactive_register_state_changed_handler(session, [](void* context, interactive_session session, interactive_state prevState, interactive_state newState)
		{
			Logger::WriteMessage(("Interactive state changed: " + std::to_string(prevState) + " -> " + std::to_string(newState)).c_str());
			static int order = 0;
			if (disconnected == prevState)
			{
				Assert::IsTrue(0 == order++ && not_ready == newState);
			}
			else if (not_ready == prevState)
			{
				Assert::IsTrue(1 == order++ && ready == newState);
			}
			else if (ready == prevState)
			{
				Assert::IsTrue(2 == order++ && not_ready == newState);
			}
		});

		ASSERT_NOERR(interactive_set_ready(session, true));
		ASSERT_NOERR(interactive_set_ready(session, false));

		// Simulate 60/fps
		const int fps = 60;
		const int seconds = 1;
		for (int i = 0; i < fps * seconds; ++i)
		{
			ASSERT_NOERR(interactive_run(session, 1));
			std::this_thread::sleep_for(std::chrono::milliseconds(1000 / fps));
		}

		Logger::WriteMessage("Disconnecting...");
		interactive_close_session(session);

		Assert::IsTrue(0 == err);
	}

	TEST_METHOD(GroupTest)
	{
		g_start = std::chrono::high_resolution_clock::now();
		interactive_config_debug(interactive_debug_trace, handle_debug_message);

		int err = 0;
		std::string clientId = CLIENT_ID;
		std::string versionId = VERSION_ID;
		std::string shareCode = SHARE_CODE;
		std::string auth;

		ASSERT_NOERR(do_auth(clientId, "", auth));

		interactive_session session;
		Logger::WriteMessage("Connecting...");
		ASSERT_NOERR(interactive_open_session(auth.c_str(), versionId.c_str(), shareCode.c_str(), true, &session));
		ASSERT_NOERR(interactive_register_participants_changed_handler(session, handle_participants_changed));

		// Simulate 60fps
		const int fps = 60;
		const int seconds = 1;
		for (int i = 0; i < fps * seconds; ++i)
		{
			ASSERT_NOERR(interactive_run(session, 1));
			std::this_thread::sleep_for(std::chrono::milliseconds(1000 / fps));
		}

		Logger::WriteMessage("Enumerating groups.");
		ASSERT_NOERR(interactive_get_groups(session, [](void* context, interactive_session session, interactive_group* group)
		{
			std::stringstream s;
			s << "[Group] '" << std::string(group->id, group->idLength) << "' Scene: '" << std::string(group->sceneId, group->sceneIdLength) << "'";
			Logger::WriteMessage(s.str().c_str());
		}));

		Logger::WriteMessage("Creating group.");
		std::string groupId = "Test group";
		ASSERT_NOERR(interactive_create_group(session, groupId.c_str(), nullptr));

		// Simulate 60fps
		for (int i = 0; i < fps * seconds; ++i)
		{
			ASSERT_NOERR(interactive_run(session, 1));
			std::this_thread::sleep_for(std::chrono::milliseconds(1000 / fps));
		}

		Logger::WriteMessage("Enumerating groups.");
		ASSERT_NOERR(interactive_get_groups(session, [](void* context, interactive_session session, interactive_group* group)
		{
			std::stringstream s;
			s << "[Group] '" << std::string(group->id, group->idLength) << "' Scene: '" << std::string(group->sceneId, group->sceneIdLength) << "'";
			Logger::WriteMessage(s.str().c_str());
		}));

		// Simulate 60 frames/sec for 1 second.
		for (int i = 0; i < fps * seconds; ++i)
		{
			ASSERT_NOERR(interactive_run(session, 1));
			std::this_thread::sleep_for(std::chrono::milliseconds(1000 / fps));
		}

		Logger::WriteMessage("Moving all participants to new group");
		interactive_get_participants(session, [](void* context, interactive_session session, interactive_participant* participant)
		{
			interactive_set_participant_group(session, participant->id, "Test group");
		});

		// Simulate 60 frames/sec for 1 second.
		for (int i = 0; i < fps * seconds; ++i)
		{
			ASSERT_NOERR(interactive_run(session, 1));
			std::this_thread::sleep_for(std::chrono::milliseconds(1000 / fps));
		}

		interactive_get_participants(session, [](void* context, interactive_session session, interactive_participant* participant)
		{
			Assert::IsTrue(0 == strcmp(participant->groupId, "Test group"));
		});

		Logger::WriteMessage("Disconnecting...");
		interactive_close_session(session);

		Assert::IsTrue(0 == err);
	}

	TEST_METHOD(ScenesTest)
	{
		g_start = std::chrono::high_resolution_clock::now();
		interactive_config_debug(interactive_debug_trace, handle_debug_message);

		int err = 0;
		std::string clientId = CLIENT_ID;
		std::string versionId = VERSION_ID;
		std::string shareCode = SHARE_CODE;
		std::string auth;

		ASSERT_NOERR(do_auth(clientId, "", auth));

		interactive_session session;
		Logger::WriteMessage("Connecting...");
		ASSERT_NOERR(interactive_open_session(auth.c_str(), versionId.c_str(), shareCode.c_str(), true, &session));
		ASSERT_NOERR(interactive_register_participants_changed_handler(session, handle_participants_changed));
		// Simulate 60 frames/sec for 1 second.
		const int fps = 60;
		const int seconds = 1;
		for (int i = 0; i < fps * seconds; ++i)
		{
			ASSERT_NOERR(interactive_run(session, 1));
			std::this_thread::sleep_for(std::chrono::milliseconds(1000 / fps));
		}

		Logger::WriteMessage("Enumerating scenes.");
		ASSERT_NOERR(interactive_get_scenes(session, [](void* context, interactive_session session, interactive_scene* scene)
		{
			std::stringstream s;
			s << "[Scene] '" << std::string(scene->id, scene->idLength) << "'";
			Logger::WriteMessage(s.str().c_str());

			Logger::WriteMessage("Groups:");
			interactive_scene_get_groups(session, scene->id, [](void* context, interactive_session session, interactive_group* group)
			{
				std::stringstream s;
				s << std::setw(10) << group->id;
				Logger::WriteMessage(s.str().c_str());
			});

			Logger::WriteMessage("Controls:");
			interactive_scene_get_controls(session, scene->id, [](void* context, interactive_session session, interactive_control* control)
			{
				print_control_properties(session, control->id);
			});
		}));

		for (int i = 0; i < fps * seconds; ++i)
		{
			ASSERT_NOERR(interactive_run(session, 1));
			std::this_thread::sleep_for(std::chrono::milliseconds(1000 / fps));
		}

		Logger::WriteMessage("Disconnecting...");
		interactive_close_session(session);

		Assert::IsTrue(0 == err);
	}
};
}