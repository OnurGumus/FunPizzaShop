Feature: Review Order


Scenario: Place Order without signing in
Given I am review
And I am not signed in
When I fill the address details
And I place the order
Then I should be asked for sign in
When I sign in
Then order should be tracked

