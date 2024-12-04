import type { UmbPropertyEditorUICollectionPermissionsElement } from './permissions.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './permissions.element.js';

export default {
	title: 'Property Editor UIs/Collection Bulk Action Permissions',
	component: 'umb-property-editor-ui-collection-permissions',
	id: 'umb-property-editor-ui-collection-permissions',
} as Meta;

export const AAAOverview: StoryFn<UmbPropertyEditorUICollectionPermissionsElement> = () =>
	html`<umb-property-editor-ui-collection-permissions></umb-property-editor-ui-collection-permissions>`;
AAAOverview.storyName = 'Overview';
