Feature: Configure Pizza

Scenario: Show Menu
When I get the main menu
Then pizza items should be fetched

Scenario: Configure Pizza
Given pizza items are fetched
When I choose a pizza
Then that pizza should be selected for configuration
