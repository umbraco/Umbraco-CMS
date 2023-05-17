# Contribution Guidelines

## Thoughts, links, and questions

In the high probability that you are porting something from angular JS then here are a few helpful tips for using Lit:

Here is the LIT documentation and playground: [https://lit.dev](https://lit.dev)

### How best to find what needs converting from the old backoffice?

1. Navigate to [https://github.com/umbraco/Umbraco-CMS](https://github.com/umbraco/Umbraco-CMS)
2. Make sure you are on the `v13/dev` branch

### What is the process of contribution?

- Read the [README](README.md) to learn how to get the project up and running
- Find an issue marked as [community/up-for-grabs](https://github.com/umbraco/Umbraco.CMS.Backoffice/issues?q=is%3Aissue+is%3Aopen+label%3Acommunity%2Fup-for-grabs) - note that some are also marked [good first issue](https://github.com/umbraco/Umbraco.CMS.Backoffice/issues?q=is%3Aissue+is%3Aopen+label%3A%22good+first+issue%22) which indicates they are simple to get started on
- Umbraco HQ owns the Management API on the backend, so features can be worked on in the frontend only when there is an API, or otherwise, if no API is required
- A contribution should be made in a fork of the repository
- Once a contribution is ready, a pull request should be made towards this repository and HQ will assign a reviewer
- A pull request should always indicate what part of a feature it tries to solve, i.e. does it close the targeted issue (if any) or does the developer expect Umbraco HQ to take over

## Contributing in general terms

A lot of the UI has already been migrated to the new backoffice. Generally speaking, one would find a feature on the projects board, locate the UI in the old backoffice (v11 is fine), convert it to Lit components using the UI library, put the business logic into a store/service, write tests, and make a pull request.

We are also very keen to receive contributions towards **documentation, unit testing, package development, accessibility, and just general testing of the UI.**

## The Management API

The management API is the colloquial term used to describe the new backoffice API. It is built as a .NET Web API, has a Swagger endpoint (/umbraco/swagger), and outputs an OpenAPI v3 schema, that the frontend consumes.

The frontend has an API formatter that takes the OpenAPI schema file and converts it into a set of TypeScript classes and interfaces.

### Caveats

1. There is currently no way to add translations. All texts in the UI are expected to be written in Umbraco’s default language of English.
2. The backoffice can be run and tested against a real Umbraco instance by cloning down the `v13/dev` branch, but there are no guarantees about how well it works yet.
3. Authentication has not been built, so the login page is never shown - HQ is working actively on this.

**Current schema for API:**

[https://raw.githubusercontent.com/umbraco/Umbraco-CMS/v13/dev/src/Umbraco.Cms.Api.Management/OpenApi.json](https://raw.githubusercontent.com/umbraco/Umbraco-CMS/v13/dev/src/Umbraco.Cms.Api.Management/OpenApi.json)

**How to convert it:**

- Run `npm run generate:api`

## A contribution example

### Example: Published Cache Status Dashboard

![Published Status Dashboard](/.github/images/contributing/published-cache-status-dashboard.png)

### Boilerplate (example using Lit)

Links for Lit examples and documentation:

- [https://lit.dev](https://lit.dev)
- [https://lit.dev/docs/](https://lit.dev/docs/)
- [https://lit.dev/playground/](https://lit.dev/playground/)

### Functionality

**HTML**

The simplest approach is to copy over the HTML from the old backoffice into a new Lit element (check existing elements in the repository, e.g. if you are working with a dashboard, then check other dashboards, etc.). Once the HTML is inside the `render` method, it is often enough to simply replace `<umb-***>` elements with `<uui-***>` and replace a few of the attributes. In general, we try to build as much UI with Umbraco UI Library as possible.

**Controller**

The old AngularJS controllers will have to be converted into modern TypeScript and will have to use our new services and stores. We try to abstract as much away as possible, and mostly you will have to make API calls and let the rest of the system handle things like error handling and so on. In the case of this dashboard, we only have a few GET and POST requests. Looking at the new Management API, we find the PublishedCacheResource, which is the new API controller to serve data to the dashboard.

To make the first button work, which simply just requests a new status from the server, we must make a call to `PublishedCacheResource.getPublishedCacheStatus()`. An additional thing here is to wrap that in a friendly function called `tryExecuteAndNotify`, which is something we make available to developers to automatically handle the responses coming from the server and additionally use the Notifications to notify of any errors:

```typescript
import { tryExecuteAndNotify } from '@umbraco-cms/resources';
import { PublishedCacheResource } from '@umbraco-cms/backend-api';

private _getStatus() {
  const { data: status } = await tryExecuteAndNotify(this, PublishedCacheResource.getPublishedCacheStatus());

  if (status) {
    // we now have the status
    console.log(status);
  }
}
```

### State (buttons, etc)

It is a good idea to make buttons indicate a loading state when awaiting an API call. All `<uui-button>` support the `.state` property, which you can set around API calls:

```typescript
@state()
private _buttonState: UUIButtonState = undefined;

private _getStatus() {
  this._buttonState = 'waiting';

  [...await...]

  this._buttonState = 'success';
}
```

## Making the dashboard visible

### Add to internal manifests

All items are declared in a `manifests.ts` file, which is located in each section directory.

To declare the Published Cache Status Dashboard as a new manifest, we need to add the section as a new json object that would look like this:

```typescript
{
 type: 'dashboard',
 alias: 'Umb.Dashboard.PublishedStatus',
 name: 'Published Status',
 elementName: 'umb-dashboard-published-status',
 loader: () => import('./backoffice/dashboards/published-status/dashboard-published-status.element'),
 meta: {
  sections: ['Umb.Section.Settings'],
  pathname: 'published-status',
  weight: 9,
 },
}
```

Let’s go through each of these properties…

- Type: can be one of the following:

  - section - examples include: `Content`, `Media`
  - dashboard - a view within a section. Examples include: the welcome dashboard
  - propertyEditorUI
  - editorView
  - propertyAction
  - tree
  - editor
  - treeItemAction

- Alias: is the unique key used to identify this item.
- Name: is the human-readable name for this item.

- ElementName: this is the customElementName declared on the element at the top of the file i.e

```typescript
@customElement('umb-dashboard-published-status')
```

- Loader: references a function call to import the file that the element is declared within

- Meta: allows us to reference additional data - in our case we can specify the section that our dashboard will sit within, the pathname that will be displayed in the url and the weight of the section

## API mock handlers

Running the app with `npm run dev`, you will quickly notice the API requests turn into 404 errors. In order to hit the API, we need to add a mock handler to define the endpoints which our dashboard will call. In the case of the Published Cache Status section, we have a number of calls to work through. Let’s start by looking at the call to retrieve the current status of the cache:

![Published Status Dashboard](/.github/images/contributing/status-of-cache.png)

From the existing functionality, we can see that this is a string message that is received as part of a `GET` request from the server.

So to define this, we must first add a handler for the Published Status called `published-status.handlers.ts` within the mocks/domains folder. In this file we will have code that looks like the following:

```typescript
const { rest } = window.MockServiceWorker;
import { umbracoPath } from '@umbraco-cms/utils';

export const handlers = [
	rest.get(umbracoPath('/published-cache/status'), (_req, res, ctx) => {
		return res(
			// Respond with a 200 status code
			ctx.status(200),
			ctx.json<string>(
				'Database cache is ok. ContentStore contains 1 item and has 1 generation and 0 snapshot. MediaStore contains 5 items and has 1 generation and 0 snapshot.'
			)
		);
	}),
];
```

This is defining the `GET` path that we will call through the resource: `/published-cache/status`

It returns a `200 OK` response and a string value with the current “status” of the published cache for us to use within the element

An example `POST` is similar. Let’s take the “Refresh status” button as an example:

![Published Status Dashboard](/.github/images/contributing/refresh-status.png)

From our existing functionality we can see that this makes a `POST`call to the server to prompt a reload of the published cache. So we would add a new endpoint to the mock handler that would look like:

```typescript
rest.post(umbracoPath('/published-cache/reload'), async (_req, res, ctx) => {
 return res(
    // Simulate a 1 second delay for the benefit of the UI
    ctx.delay(1000)
    // Respond with a 201 status code
    ctx.status(201)
 );
})
```

Which is defining a new `POST` endpoint that we can add to the core API fetcher using the path `/published-cache/reload`.

This call returns a simple `OK` status code and no other object.

## Storybook stories

We try to make good Storybook stories for new components, which is a nice way to work with a component in an isolated state. Imagine you are working with a dialog on page 3 and have to navigate back to that every time you make a change - this is now eliminated with Storybook as you can just make a story that displays that step. Storybook can only show one component at a time, so it also helps us to isolate view logic into more and smaller components, which in turn are more testable.

In depth: [https://storybook.js.org/docs/web-components/get-started/introduction](https://storybook.js.org/docs/web-components/get-started/introduction)

Reference: [https://ambitious-stone-0033b3603.1.azurestaticapps.net/](https://ambitious-stone-0033b3603.1.azurestaticapps.net/)

- Locally: `npm run storybook`

For Umbraco UI stories, please navigate to [https://uui.umbraco.com/](https://uui.umbraco.com/)

## Testing

There are two testing tools on the backoffice: unit testing and end-to-end testing.

### Unit testing

We are using a tool called Web Test Runner which spins up a bunch of browsers using Playwright with the well-known jasmine/chai syntax. It is expected that any new component/element has a test file named “&lt;component>.test.ts”. It will automatically be picked up and there are a set of standard tests we apply to all components, which checks that they are registered correctly and they pass accessibility testing through Axe.

Working with playwright: [https://playwright.dev/docs/intro](https://playwright.dev/docs/intro)

### End-to-end testing

This test is being performed by Playwright as well but is running in a mode where Playwright clicks through the browser in different scenarios and reports on those. There are no requirements to add these tests to new components yet, but it is encouraged. The tests are located in a separate app called “backoffice-e2e”.

## Putting it all together

When we are finished with the dashboard we will hopefully have something akin to this [real-world example of the actual dashboard that was migrated](https://github.com/umbraco/Umbraco.CMS.Backoffice/tree/main/src/backoffice/settings/dashboards/published-status).
