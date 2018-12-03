describe('my app', function() {

  beforeEach(function() {
    browser().navigateTo('/');
  });

  it('should be publicly accessible and default route to be /content', function() {
    expect(browser().location().path()).toBe("/content");
  });
});