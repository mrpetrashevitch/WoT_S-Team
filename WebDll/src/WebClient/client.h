#pragma once
#include "../defs.h"

#include "winsock2.h"
//#include "winsock.h"

#include <Windows.h>
#include <mutex>

namespace web_client
{

	enum class WebActions
	{
		LOGIN = 1,
		LOGOUT = 2,
		MAP = 3,
		GAME_STATE = 4,
		GAME_ACTIONS = 5,
		TURN = 6,
		CHAT = 100,
		MOVE = 101,
		SHOOT = 102,
	};

	class client
	{
		bool _inited;
		bool _connected;
		SOCKET _socket;
		SOCKADDR_IN _addr;
		//std::mutex _mut;

		Result _init();
		bool _send_all(void* buf, int len);
		bool _recv_all(void* buf, int len);
	public:
		client();
		~client();


		Result connect(const SOCKADDR_IN& addr);
		Result detach();
		Result send_packet(WebActions action, int size, byte* data, int* out_size, byte* out_data);
	};

}
