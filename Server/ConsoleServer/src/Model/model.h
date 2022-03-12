#pragma once
#include "../Json/json.hpp"
#include <string>


enum Action
{
    LOGIN = 1,
    LOGOUT = 2,
    MAP = 3,
    GAME_STATE = 4,
    GAME_ACTIONS = 5,
    TURN = 6,
    CHAT = 100,
    MOVE = 101,
    SHOOT = 102,
};

enum Result
{
    OKEY = 0,
    BAD_COMMAND = 1,
    ACCESS_DENIED = 2,
    INAPPROPRIATE_GAME_STATE = 3,
    TIMEOUT = 4,
    INTERNAL_SERVER_ERROR = 500,
};

struct login
{
	std::string name;
	std::string password;
	std::string game;
	int num_turns;
	int num_players;
	bool is_observer;
};


void to_json(nlohmann::json& j, const login& p);
void from_json(const nlohmann::json& j, login& p);
