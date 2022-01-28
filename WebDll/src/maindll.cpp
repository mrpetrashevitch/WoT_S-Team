#include "WebClient/client.h"

extern "C"
{
	__declspec(dllexport) web_client::client* create()
	{
		web_client::client* web = nullptr;
		try
		{
			web = new web_client::client();
			return web;
		}
		catch (const std::exception&)
		{
			return nullptr;
		}
	}

	__declspec(dllexport) web_client::Result connect_(web_client::client* web, uint addr, ushort port)
	{
		if (!web) return web_client::Result::IVALID_PARAM;
		SOCKADDR_IN serv_adr;
		serv_adr.sin_addr.s_addr = addr;
		serv_adr.sin_port = htons(port);
		serv_adr.sin_family = AF_INET;
		return web->connect(serv_adr);
	}

	__declspec(dllexport) web_client::Result send_packet(web_client::client* web, web_client::WebActions action, int size, byte* data, int* out_size, byte* out_data)
	{
		if (!web) return web_client::Result::IVALID_PARAM;
		return web->send_packet(action, size, data, out_size, out_data);
	}

	__declspec(dllexport) web_client::Result destroy(void* web)
	{
		if (!web) return web_client::Result::IVALID_PARAM;
		try
		{
			delete web;
			return web_client::Result::OKEY;
		}
		catch (const std::exception&)
		{
			return web_client::Result::IVALID_PARAM;
		}
	}
}