#include "WebClient/client.h"
#include "AI/ai.h"

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

	__declspec(dllexport) Result connect_(web_client::client* web, uint addr, ushort port)
	{
		if (!web) return Result::IVALID_PARAM;
		SOCKADDR_IN serv_adr;
		serv_adr.sin_addr.s_addr = addr;
		serv_adr.sin_port = htons(port);
		serv_adr.sin_family = AF_INET;
		return web->connect(serv_adr);
	}

	__declspec(dllexport) Result send_packet(web_client::client* web, web_client::WebActions action, int size, byte* data, int* out_size, byte* out_data)
	{
		if (!web) return Result::IVALID_PARAM;
		return web->send_packet(action, size, data, out_size, out_data);
	}

	__declspec(dllexport) Result destroy(web_client::client* web)
	{
		if (!web) return Result::IVALID_PARAM;
		try
		{
			delete web;
			return Result::OKEY;
		}
		catch (const std::exception&)
		{
			return Result::IVALID_PARAM;
		}
	}

	__declspec(dllexport) Result get_action(int curr_player,
		ai::player_native* players, int players_size,
		ai::vehicles_native* vehicle, int vehicle_size,
		ai::win_points_native* win_points, int win_points_size,
		ai::AttackMatrix_native* attack_matrix, int attack_matrix_size,
		ai::point* base, int base_size, ai::action_ret* out_actions)
	{
		return ai::ai::get_action(curr_player,
			players, players_size,
			vehicle, vehicle_size,
			win_points, win_points_size,
			attack_matrix, attack_matrix_size,
			base, base_size, out_actions);
	}
}