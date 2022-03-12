#pragma once
#include "../../../WGLib/src/Packets/packets.h"

namespace packets
{
	enum class packet_type : int32
	{
		packet_string = 0,
		packet_data,
	};

	class packet_data : public web::packet::i_packet_network
	{
	public:
		packet_data(const void* body, const int data_size);
		byte body[WEB_BASE_PACKET_MAX_SIZE];
	};
}
