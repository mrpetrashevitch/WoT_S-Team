#include "packets.h"
#include <string>


namespace packets
{

	packet_data::packet_data(const void* body, const int data_size)
	{
		_header.type = static_cast<int32>(packet_type::packet_data);
		if (!memcpy_s(&this->body, sizeof(this->body), body, data_size))
			_header.size += data_size;
	}
}
