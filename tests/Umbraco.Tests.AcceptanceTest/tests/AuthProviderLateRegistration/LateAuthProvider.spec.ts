import {test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Regression guard for the v17.4+ external-login race (introduced in #22522).
//
// AdditionalSetup/App_Plugins/LateAuthProvider deploys an appEntryPoint whose onInit, after a
// 1.5s delay, registers an external auth provider ("Late External Login"). This mirrors a real
// provider (e.g. Umbraco ID) that registers its authProvider during an async onInit on a slow
// connection.
//
// The backoffice boot must wait for app-entry-points to settle before rendering the login
// screen. If it does not, the login decision is made before the provider is registered, the
// late provider never appears, and the user is dropped on the local login instead. So: the
// late-registered provider MUST be offered on the login screen.
//
// This test is intentionally brittle (it depends on boot timing) but must remain working — it
// is the only end-to-end guard for the boot-gate behaviour.
test('a late-registered external auth provider is offered on the login screen', async ({umbracoUi}) => {
  test.slow();

  // Act - navigate to the backoffice unauthenticated (the login screen).
  await umbracoUi.goToBackOffice();

  // Assert - the provider registered late by the appEntryPoint is still offered. On the
  // buggy boot the login screen renders before this provider exists, so it never appears.
  const lateProviderButton = umbracoUi.page
    .locator('umb-auth-provider-default')
    .getByText('Sign in with Late External Login');
  await expect(lateProviderButton).toBeVisible({timeout: 15000});
});
