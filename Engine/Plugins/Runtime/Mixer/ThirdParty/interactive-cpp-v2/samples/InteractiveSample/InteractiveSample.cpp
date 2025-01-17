#include "stdafx.h"
#include "../../source/interactivity.h"

#include <windows.h>
#include <shellapi.h>

// STL includes
#include <chrono>
#include <thread>
#include <iostream>
#include <map>
#include <string>

#define CLIENT_ID		"f0d20e2d263b75894f5cdaabc8a344b99b1ea6f9ecb7fa4f"
#define INTERACTIVE_ID	"135704"
#define SHARE_CODE		"xe7dpqd5"

// Called ~60 times per second
int update(interactive_session session)
{
	// This call processes any waiting messages from the interactive service. If there are no messages this returns immediately.
	// All previously registered session callbacks will be called from this thread.
	return interactive_run(session, 1);
}

// Display a short code to the user in order to obtain a reusable token for future connections.
int authorize(std::string& authorization)
{
	int err = 0;
	char shortCode[7];
	size_t shortCodeLength = sizeof(shortCode);
	char shortCodeHandle[1024];
	size_t shortCodeHandleLength = sizeof(shortCodeHandle);
	err = interactive_auth_get_short_code(CLIENT_ID, nullptr, shortCode, &shortCodeLength, shortCodeHandle, &shortCodeHandleLength);
	if (err) return err;

	// Pop the browser for the user to approve access.
	std::string authUrl = std::string("https://www.mixer.com/go?code=") + shortCode;
	ShellExecuteA(0, 0, authUrl.c_str(), nullptr, nullptr, SW_SHOW);

	// Wait for OAuth token response.
	char refreshTokenBuffer[1024];
	size_t refreshTokenLength = sizeof(refreshTokenBuffer);
	err = interactive_auth_wait_short_code(CLIENT_ID, nullptr, shortCodeHandle, refreshTokenBuffer, &refreshTokenLength);
	if (err)
	{
		if (MIXER_ERROR_TIMED_OUT == err)
		{
			std::cout << "Authorization timed out, user did not approve access within the time limit.";
		}
		else if (MIXER_ERROR_AUTH_DENIED == err)
		{
			std::cout << "User denied access.";
		}

		return err;
	}

	/*
	*	TODO:	This is where you would serialize the refresh token locally or on your own service for future use in a way that is associated with the current user.
	*			Future calls would then only need to check if the token is stale, refresh it if so, and then parse the new authorization header.
	*/

	// Extract the authorization header from the refresh token.
	char authBuffer[1024];
	size_t authBufferLength = sizeof(authBuffer);
	err = interactive_auth_parse_refresh_token(refreshTokenBuffer, authBuffer, &authBufferLength);
	if (err) return err;

	authorization = std::string(authBuffer, authBufferLength);
	return 0;
}

std::map<std::string, std::string> controlsByTransaction;

int main()
{
	int err = 0;

	// Get an authorization token for the user to pass to the connect function.
	std::string authorization;
	err = authorize(authorization);
	if (err) return err;

	// Connect to the user's interactive channel, using the interactive project specified by the version ID.
	interactive_session session;
	err = interactive_open_session(authorization.c_str(), INTERACTIVE_ID, SHARE_CODE, true, &session);
	if (err) return err;

	// Register a callback for button presses.
	err = interactive_register_input_handler(session, [](void* context, interactive_session session, const interactive_input* input)
	{	
		if ((input_type_key == input->type || input_type_click == input->type) && interactive_button_action_down == input->buttonData.action)
		{
			// Capture the transaction on button down to deduct sparks
			controlsByTransaction[input->transactionId] = input->control.id;
			interactive_capture_transaction(session, input->transactionId);
		}
	});
	if (err) return err;

	// Register a callback for completed transactions.
	err = interactive_register_transaction_complete_handler(session, [](void* context, interactive_session session, const char* transactionId, size_t transactionIdLength, unsigned int errorCode, const char* errorMessage, size_t errorMessageLength)
	{
		if (!errorCode)
		{
			// Transaction was captured, now execute the most super awesome interactive functionality!
			std::string controlId = controlsByTransaction[std::string(transactionId, transactionIdLength)];
			if (0 == strcmp("GiveHealth", controlId.c_str()))
			{
				std::cout << "Giving health to the player!" << std::endl;
			}
		}
		else
		{
			std::cerr << errorMessage << "(" << std::to_string(errorCode) << ")" << std::endl;
		}

		controlsByTransaction.erase(transactionId);
	});

	if (err) return err;
	
	// Simulate game update loop. All callbacks will be called from this thread.
	for (;;)
	{
		err = update(session);
		if (err) break;
		std::this_thread::sleep_for(std::chrono::milliseconds(16));
	}

	interactive_close_session(session);

    return err;
}