Feature: Login

Scenario: Login Screen valid input
Given I am at login screen
When I typed a valid email address
Then it should ask me verification code

Scenario: Login Screen valid input2
Given I am at login screen
When I typed a valid email address
Then it should ask me verification code

# Scenario: Verification
# Given I started with a calculation
# And I am at the verification screen
# When I provided a valid valid verification code
# Then it should execute the calculation