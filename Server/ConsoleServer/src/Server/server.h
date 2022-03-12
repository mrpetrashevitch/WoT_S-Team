#pragma once
#include "../../../WGserver/src/include.h"
#include "../Model/model.h"


#include <string>
#include <vector>
#include <mutex>
#include <atomic>
#pragma warning(disable : 4996)


namespace server
{
	/*std::string current_date_time()
	{
		time_t     now = time(0);
		struct tm  tstruct;
		char       buf[80];
		tstruct = *localtime(&now);
		strftime(buf, sizeof(buf), "%Y-%m-%d.%X", &tstruct);
		return buf;
	}*/

	class server
	{
		web::io_server::i_server* _server = nullptr;


		std::recursive_mutex mut_conn;
		std::vector<web::io_base::i_connection*> _connestions;

		std::atomic<unsigned long long> total_packet_r = 0;
		std::atomic<unsigned long long> total_packet_s = 0;
		std::atomic<unsigned long long> total_sended_b = 0;
		std::atomic<unsigned long long> total_recved_b = 0;
		std::atomic<int> total_connection = 0;

		void cb_on_accepted(web::io_base::i_connection* conn, const SOCKET& socket)
		{
			total_connection++;
			printf("accepted: socket %d, total %d\n", socket, total_connection.load());

		}
		void cb_on_recv(web::io_base::i_connection* conn, web::packet::packet_network* packet)
		{
			total_recved_b += packet->header.size + sizeof(packet->header);
			total_packet_r++;

			std::string str_json((char*)packet->body, packet->header.size);
			nlohmann::json js_parser = nlohmann::json::parse(str_json);

			try
			{
				if (packet->header.type == Action::LOGIN)
				{
					auto data = js_parser.get<login>();



				}
			}
			catch (const std::exception& ex)
			{
				printf(ex.what());
			}



			int a = 100;
			//SOCKET sock = conn->get_socket();
			/*{
				std::lock_guard<std::mutex> lg(mut_cout);
				std::cout << current_date_time() << " on_recved from " << sock << ": size " << packet->size << std::endl;
			}*/

			/*if (web::packet::get_packet_type(packet_nt) == web::packet::packet_type::packet_str)
			{
				auto packet = web::packet::packet_cast<web::packet::packet_str>(packet_nt);
				std::cout << current_date_time() << " " << packet->str << std::endl;
			}*/

			/*if (packet->packet.type == web::web_base::packet_type::str)
			{
				std::string str("[user");
				str += std::to_string(sock);
				str += "]: ";
				str += (const char*)packet->packet.body;

				std::shared_ptr<web::web_base::packet_str> p(std::make_shared<web::web_base::packet_str>(str.c_str()));
				web::web_server::i_server* server = reinterpret_cast<web::web_server::i_server*>(conn->get_owner());
				{
					std::lock_guard<std::recursive_mutex> lg(mut_conn);
					for (auto& i : _connestions)
						server->send_packet_async(i, p);
				}
			}*/
		}
		void cb_on_send(web::io_base::i_connection* conn, web::packet::packet_network* packet)
		{
			total_sended_b += packet->header.size + sizeof(packet->header);
			total_packet_s++;
		}
		void cb_on_disconnected(web::io_base::i_connection* conn)
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

	public:
		server(web::io_server::i_server* server) : _server(server)
		{
			_server->set_on_accepted(std::bind(&server::cb_on_accepted, this, std::placeholders::_1, std::placeholders::_2));
			_server->set_on_recv(std::bind(&server::cb_on_recv, this, std::placeholders::_1, std::placeholders::_2));
			_server->set_on_send(std::bind(&server::cb_on_send, this, std::placeholders::_1, std::placeholders::_2));
			_server->set_on_disconnected(std::bind(&server::cb_on_disconnected, this, std::placeholders::_1));
		}

		bool run()
		{
			return _server->run();
		}

	};
}
