#pragma once
#include "../defs.h"
#include <cmath>
#include <algorithm>

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

	struct player_native
	{
		int idx;
		int is_observer;
	};

	enum class vehicle_type : int
	{
		MT,
		LT,
		HT,
		ASPG,
		SPG
	};

	struct vehicles_native
	{
		int vehicle_id;
		int player_id;
		vehicle_type vehicle_type;
		int health;
		point spawn_position;
		point position;
		int capture_points;
	};

	struct win_points_native
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
	private:
		static void get_targets(int curr_player,
			vehicles_native* vehicles, int vehicles_size,
			point position,
			vehicles_native** targets, int* targets_size);
		static int distance(point a, point b);
		static bool check_neutrality(int curr_player, int goal,
			AttackMatrix_native* attack_matrix, int attack_matrix_size);
	public:
		static Result get_action(int curr_player,
			player_native* players, int players_size,
			vehicles_native* vehicles, int vehicles_size,
			win_points_native* win_points, int win_points_size,
			AttackMatrix_native* attack_matrix, int attack_matrix_size,
			point* base, int base_size, action_ret* out_actions);
	};
}