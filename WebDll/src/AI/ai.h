#pragma once
#include "../defs.h" 

namespace ai
{
	enum class action_type : int
	{
		nun,
		move,
		shoot
	};

	struct point
	{
		int x, y, z;
	};


	struct action
	{
		action_type action_type;
		int vec_id;
		point point;
	};

	struct action_ret
	{
		action actions[5];
	};

	struct Player_native
	{
		int idx;
		int is_observer;
	};

	enum class VehicleType : int
	{
		MT,
		LT,
		HT,
		ASPG,
		SPG
	};

	struct Vehicle_native
	{
		int vehicle_id;
		int player_id;
		VehicleType vehicle_type;
		int health;
		point spawn_position;
		point position;
		int capture_points;
	};

	struct WinPoints_native
	{
		int id;
		int capture;
		int kill;
	};

	struct AttackMatrix_native
	{
		int id;
		int attack[3];
	};

	class ai
	{
	public:
		static Result get_action(int curr_player,
			Player_native* players, int players_size,
			Vehicle_native* vehicle, int vehicle_size,
			WinPoints_native* win_points, int win_points_size,
			AttackMatrix_native* attack_matrix, int attack_matrix_size, action_ret* out_actions);
	};
}