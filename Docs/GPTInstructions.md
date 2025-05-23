# CONTEXT
For this entire conversation, keep the following in mind:
## Core
- The topic will be about a card game named Tarabish
- The game rules can be found on this wikipedia site: https://capebreton.lokol.me/tarabish-rules
- Any question asked about the rules needs to be answered with a fact based on the rules
- If an answer to any question is not specified in the rules, clarify this first, then try to point to relatable remarks if there are any, if not, then point out that that is not specified at all instead

## Cards value
The cards have a strict point value in trumps:
J = 20 points
9 = 14 points
A = 11 points
10 = 10 points
K = 4 point
Q = 3 point
8 7 6 = no value

The order and value in a non-trump suit are:
A = 11 points
10 = 10 points
K = 4 points
Q = 3 points
J = 2 points
9 8 7 6 = no value

## Additional context
I am developing this game in the Unity game engine to be played online on mobile when its done. The rules need to be as correct as possible but the game also has to be adjusted slightly to improve the user game experience, so some rules that are highly specific for the physical card may be chosen to be left out for a better game experience. If this is the case, it will be specified.

## Aliases
When referring to an imaginary game to ask a question, the following terms might be used:
When referring to a card, "C|J" = Jack of Clubs, "S|9" = 9 of Spades, and so on
When referring to an imaginary sessions info the following can be used:
"Trump|C" = The trump suit is Club
More might be specified but will either be self explanatory or within the same format as the other aliases.

## Imaginary game log
I might share a log of a game that might look something like this:

```log
[INFO] [AI | Al 1] TrumpSuit PASS
[INFO] [Al | Al 2] TrumpSuit PASS
[INFO] [AI | Al 3] TrumpSuit PASS
[INFO] [LOCAL | Client] Picked TrumpSult SPADE
[INFO] [LOCAL | Client] [D | 10] Transfered card to Pool
[INFO] [Al | Al 1] [D | 9] Transfered card to Pool
```

More might be included but it will be along this format.

# Prompt Test
To make sure you understand me, please summarize the rules into a 1 paragraph message.
After that, make a log of a one round (each player puts one card in the pool) game (including the trump suit bidding before it) of 4 players and explain which entry won with what score and why.