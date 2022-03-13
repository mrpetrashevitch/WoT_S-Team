#include "server.h"
#include "../Json/json_extensions.h"
#include "../Packets/packets.h"

namespace server
{
	void server::_cb_on_accepted(web::io_base::i_connection* conn, const SOCKET& socket)
	{
		total_connection++;
		printf("accepted: socket %d, total %d\n", socket, total_connection.load());

	}

	void server::_cb_on_recv(web::io_base::i_connection* conn, web::packet::packet_network* packet)
	{
		total_recved_b += packet->header.size + sizeof(packet->header);
		total_packet_r++;

		std::string str_json((char*)packet->body, packet->header.size);
		nlohmann::json js_parser = nlohmann::json::parse(str_json);

		models::game_state gs;
		gs.attack_matrix.insert(std::pair<int, std::vector<int>>(123, { 3,3 }));
		gs.attack_matrix.insert(std::pair<int, std::vector<int>>(3, { 1,2 }));
		const nlohmann::json j11{ gs };
		const auto s11 = j11[0].dump(4);

		models::map map;
		map.size = 11;
		map.name = "map01";
		map.spawn_points.push_back({ {{ 1,2,3 }},{{4,5,6}},{{7,8,9}},{{10,11,12}},{{13,14,15}} });
		map.spawn_points.push_back({ {{ 10,20,30 }},{{40,50,60}},{{70,80,90}},{{100,110,120}},{{130,140,150}} });
		map.content.base.push_back({ 1,2,3 });
		map.content.base.push_back({ 333,2,3 });
		map.content.obstacle.push_back({ 5,5,5 });
		const nlohmann::json j1{ map };
		const auto s1 = j1[0].dump(4);

		if (packet->header.type == models::Action::LOGIN)
		{
			models::login data;
			try
			{
				data = js_parser.get<models::login>();
			}
			catch (const std::exception& ex)
			{
				printf(ex.what());
			}

			auto [res,player] = _engine.login(data, (int)conn->get_socket());

			if (res != models::Result::OKEY)
			{
				_server->send_packet_async(conn, std::make_shared<packets::packet_error>(res));
			}
			else
			{
				const nlohmann::json j_player{ player };
				const auto str_player = j_player[0].dump(4);
				_server->send_packet_async(conn, std::make_shared<packets::packet_json>(res, str_player));
			}
		}
		else if (packet->header.type == models::Action::MAP)
		{
			auto [res, player] = _engine.map((int)conn->get_socket());
		}



		int a = 100;

	}

	void server::_cb_on_send(web::io_base::i_connection* conn, web::packet::packet_network* packet)
	{
		total_sended_b += packet->header.size + sizeof(packet->header);
		total_packet_s++;
	}

	void server::_cb_on_disconnected(web::io_base::i_connection* conn)
	{
		total_connection--;
		printf("disconnected: socket %d, total %d\n", conn->get_socket(), total_connection.load());


		std::string str("[user");
		str += std::to_string(conn->get_socket());
		str += "] ";
		str += " has been disconnected";

		std::shared_ptr<web::packet::packet_string> p(std::make_shared<web::packet::packet_string>(str.c_str()));
		web::io_server::i_server* server = reinterpret_cast<web::io_server::i_server*>(conn->get_owner());

		{
			std::lock_guard<std::recursive_mutex> lg(mut_conn);
			auto item = std::find_if(_connestions.begin(), _connestions.end(), [&conn](web::io_base::i_connection* c) { return c->get_socket() == conn->get_socket(); });
			if (item != _connestions.end())
				_connestions.erase(item);

			/*for (auto& i : _connestions)
			if (i->get_socket() != conn->get_socket())
			{
			server->send_packet_async(i, p);
			}*/
		}
	}

	server::server(web::io_server::i_server* server) : _server(server)
	{
		_server->set_on_accepted(std::bind(&server::_cb_on_accepted, this, std::placeholders::_1, std::placeholders::_2));
		_server->set_on_recv(std::bind(&server::_cb_on_recv, this, std::placeholders::_1, std::placeholders::_2));
		_server->set_on_send(std::bind(&server::_cb_on_send, this, std::placeholders::_1, std::placeholders::_2));
		_server->set_on_disconnected(std::bind(&server::_cb_on_disconnected, this, std::placeholders::_1));
	}

	bool server::run()
	{
		if (_server->run() && _engine.run())
			return true;
		return false;
	}
}