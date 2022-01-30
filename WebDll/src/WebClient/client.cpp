#include "client.h"
#include <WinInet.h>

#include <Ws2tcpip.h>
#include <mswsock.h>

//#pragma warning(disable: 4996)
//#pragma warning(disable: 4200)

namespace web_client
{
	Result client::_init()
	{
		if (_inited) return Result::OKEY;

		WSADATA wsaData;
		if (WSAStartup(WINSOCK_VERSION, &wsaData))
			return Result::ERROR_WSA_INIT;
		_inited = true;
		return Result::OKEY;
	}

	bool client::_send_all(void* buf, int len)
	{
		int total = 0;
		int bytesleft = len;
		int n;
		while (total < len)
		{
			n = send(_socket, reinterpret_cast<char*>(buf) + total, bytesleft, 0);
			if (n == -1 || n == 0)
			{
				detach();
				return false;
			}
			total += n;
			bytesleft -= n;
		}
		return true;
	}

	bool client::_recv_all(void* buf, int len)
	{
		auto n = ::recv(_socket, reinterpret_cast<char*>(buf), len, MSG_WAITALL);
		if (n > 0) return true;
		detach();
		return false;
	}

	client::client(): _inited(false), _connected(false), _socket(0)
	{
		ZeroMemory(&_addr, sizeof(_addr));
	}

	client::~client()
	{
		WSACleanup();
	}

	
	Result client::connect(const SOCKADDR_IN& addr)
	{
		Result res;
		if ((res = _init()) != Result::OKEY)
			return res;

		detach();
		SOCKET socket;
		if (INVALID_SOCKET == (socket = ::socket(AF_INET, SOCK_STREAM, IPPROTO_TCP)))
			return Result::ERROR_SOCKET;

		if (::connect(socket, (SOCKADDR*)&addr, sizeof(addr)) != 0)
		{
			closesocket(socket);
			return Result::ERROR_CONNECT;
		}
		_socket = socket;
		_addr = addr;
		_connected = true;
		return Result::OKEY;
	}
	Result client::detach()
	{
		if (_connected)
		{
			closesocket(_socket);
			_socket = 0;
			return Result::OKEY;
		}
		return Result::OKEY;
	}
	Result client::send_packet(WebActions action, int size, byte* data, int* out_size, byte* out_data)
	{
		if (!_connected) return Result::CONNECTED_FALSE;
		if (!_send_all(&action, sizeof(action)) ||
			!_send_all(&size, sizeof(size)))
			return Result::ERROR_SEND;

		if (size)
			if (!_send_all(data, size))
				return Result::ERROR_SEND;

		Result res;
		int packet_size;
		if (!_recv_all(&res, sizeof(res)) ||
			!_recv_all(&packet_size, sizeof(packet_size)))
			return Result::ERROR_RECV;

		if (packet_size == 0)
		{
			*out_size = packet_size;
			return res;
		}

		if (!_recv_all(out_data, packet_size))
			return Result::ERROR_RECV;
		*out_size = packet_size;
		return res;
	}
}