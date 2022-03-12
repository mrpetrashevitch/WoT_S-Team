#include "Server/server.h"
#include <sstream> //std::stringstream
#include <iostream>

void Clear()
{
#if defined _WIN32
	system("cls");
	//clrscr(); // including header file : conio.h
#elif defined (__LINUX__) || defined(__gnu_linux__) || defined(__linux__)
	system("clear");
	//std::cout<< u8"\033[2J\033[1;1H"; //Using ANSI Escape Sequences 
#elif defined (__APPLE__)
	system("clear");
#endif
}



int main(int argc, char* argv[])
{
	if (argc < 2) return 1;

	int b0, b1, b2, b3, port;
	{
		std::string ip = argv[1];
		std::stringstream s(ip);
		char ch;
		s >> b0 >> ch >> b1 >> ch >> b2 >> ch >> b3 >> ch >> port;
		printf("input address: %d.%d.%d.%d:%d\n", b0, b1, b2, b3, port);
	}

	const char* path = "web_server.dll";

	web::io_server::web_server_dll_loader ld;
	ld.load(path);

	web::io_server::i_server* server_cl = ld.create_fn(b0, b1, b2, b3, port);

	server::server server(server_cl);
	server.run();

	int last = 0;

	/*for (;;)
	{
		printf("Total connection: %d, total packets: %d (%d)                     \r", total_connection.load(), total_packet.load(), total_packet.load() - last);
		last = total_packet;
		Sleep(1000);
	}*/

	std::cin.get();
	return 0;
}