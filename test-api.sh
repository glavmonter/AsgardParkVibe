#!/bin/bash

# Parking Gate API Test Script
# Требуется: curl, jq (опционально для форматирования JSON)

BASE_URL="https://localhost:7001"
# Для HTTP используйте:
# BASE_URL="http://localhost:5001"

# Цвета для вывода
GREEN='\033[0;32m'
BLUE='\033[0;34m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo -e "${BLUE}=== Parking Gate API Test Script ===${NC}\n"

# Функция для проверки статуса
check_status() {
    echo -e "${BLUE}Checking gate status...${NC}"
    curl -k -s -X GET "$BASE_URL/api/status" | jq . 2>/dev/null || curl -k -s -X GET "$BASE_URL/api/status"
    echo -e "\n"
}

# Функция для паузы
pause() {
    echo -e "${GREEN}Press Enter to continue...${NC}"
    read
}

# 1. Health Check
echo -e "${BLUE}1. Health Check${NC}"
curl -k -s -X GET "$BASE_URL/api/status/health" | jq . 2>/dev/null || curl -k -s -X GET "$BASE_URL/api/status/health"
echo -e "\n"
pause

# 2. Начальное состояние
echo -e "${BLUE}2. Initial State${NC}"
check_status
pause

# 3. Сценарий успешного въезда
echo -e "${GREEN}=== Scenario: Successful Entry ===${NC}\n"

echo -e "${BLUE}3.1. Vehicle Approaches${NC}"
curl -k -s -X POST "$BASE_URL/api/simulation/vehicle/approached" | jq . 2>/dev/null || curl -k -s -X POST "$BASE_URL/api/simulation/vehicle/approached"
echo -e "\n"
sleep 1
check_status
pause

echo -e "${BLUE}3.2. Card Read (with access)${NC}"
curl -k -s -X POST "$BASE_URL/api/simulation/card/read" \
  -H "Content-Type: application/json" \
  -d '{"cardNumber": "1234"}' | jq . 2>/dev/null || curl -k -s -X POST "$BASE_URL/api/simulation/card/read" \
  -H "Content-Type: application/json" \
  -d '{"cardNumber": "1234"}'
echo -e "\n"
sleep 1
check_status
pause

echo -e "${BLUE}3.3. Vehicle Passes Through${NC}"
curl -k -s -X POST "$BASE_URL/api/simulation/vehicle/passed-through" | jq . 2>/dev/null || curl -k -s -X POST "$BASE_URL/api/simulation/vehicle/passed-through"
echo -e "\n"
sleep 1
check_status
pause

# 4. Сценарий отказа в доступе
echo -e "${RED}=== Scenario: Access Denied ===${NC}\n"

echo -e "${BLUE}4.1. Vehicle Approaches${NC}"
curl -k -s -X POST "$BASE_URL/api/simulation/vehicle/approached" | jq . 2>/dev/null || curl -k -s -X POST "$BASE_URL/api/simulation/vehicle/approached"
echo -e "\n"
sleep 1
check_status
pause

echo -e "${BLUE}4.2. Card Read (without access)${NC}"
curl -k -s -X POST "$BASE_URL/api/simulation/card/read" \
  -H "Content-Type: application/json" \
  -d '{"cardNumber": "9999"}' | jq . 2>/dev/null || curl -k -s -X POST "$BASE_URL/api/simulation/card/read" \
  -H "Content-Type: application/json" \
  -d '{"cardNumber": "9999"}'
echo -e "\n"
sleep 1
check_status
pause

echo -e "${BLUE}4.3. Vehicle Departs${NC}"
curl -k -s -X POST "$BASE_URL/api/simulation/vehicle/departed" | jq . 2>/dev/null || curl -k -s -X POST "$BASE_URL/api/simulation/vehicle/departed"
echo -e "\n"
sleep 1
check_status
pause

# 5. Сценарий движения назад
echo -e "${BLUE}=== Scenario: Vehicle Backs Out ===${NC}\n"

echo -e "${BLUE}5.1. Vehicle Approaches${NC}"
curl -k -s -X POST "$BASE_URL/api/simulation/vehicle/approached" | jq . 2>/dev/null || curl -k -s -X POST "$BASE_URL/api/simulation/vehicle/approached"
echo -e "\n"
sleep 1

echo -e "${BLUE}5.2. Card Read (with access)${NC}"
curl -k -s -X POST "$BASE_URL/api/simulation/card/read" \
  -H "Content-Type: application/json" \
  -d '{"cardNumber": "1111"}' | jq . 2>/dev/null || curl -k -s -X POST "$BASE_URL/api/simulation/card/read" \
  -H "Content-Type: application/json" \
  -d '{"cardNumber": "1111"}'
echo -e "\n"
sleep 1

echo -e "${BLUE}5.3. Vehicle Backs Out${NC}"
curl -k -s -X POST "$BASE_URL/api/simulation/vehicle/backed-out" | jq . 2>/dev/null || curl -k -s -X POST "$BASE_URL/api/simulation/vehicle/backed-out"
echo -e "\n"
sleep 1
check_status

echo -e "${GREEN}=== Testing Complete ===${NC}"
