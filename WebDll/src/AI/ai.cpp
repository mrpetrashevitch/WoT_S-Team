#include "ai.h"


namespace ai
{
	Result ai::get_action(int curr_player,
		player_native* players, int players_size,
		vehicles_native* vehicles, int vehicles_size,
		win_points_native* win_points, int win_points_size,
		AttackMatrix_native* attack_matrix, int attack_matrix_size, action_ret* out_actions)
	{
	
			for (int j = 0, i = 0; j < vehicles_size; j++)
			{
				if (vehicles[j].player_id == curr_player)
				{
					out_actions->actions[i].action_type = action_type::move;
					out_actions->actions[i].vec_id = vehicles[j].vehicle_id;
					out_actions->actions[i].point = vehicles[j].position;
					out_actions->actions[i].point.x++;
					out_actions->actions[i].point.z--;
					i++;
				}
			}
		return Result::OKEY;
	}
}
