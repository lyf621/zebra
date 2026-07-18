# Waiting To Merge Branch

This branch is a merge-friendly presentation version for team integration.

- `IntegrationPlaceholderMode.Enabled` is `true`.
- The paper board background and card content are hidden.
- Every temporary card can use any empty location.
- Playing and retaining cards keep their animations and discard flow, but do not change resources.

When the team is ready to connect real card and location data, set `IntegrationPlaceholderMode.Enabled` to `false`. The existing `CardModel`, draw/discard flow, card interaction, and location-placement flow remain unchanged.
