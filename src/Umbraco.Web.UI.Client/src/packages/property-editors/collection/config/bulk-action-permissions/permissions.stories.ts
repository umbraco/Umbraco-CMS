import type { UmbPropertyEditorUICollectionPermissionsElement } from './permissions.element.js';
import type { Meta, StoryFn } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './permissions.element.js';

export default {
	title: 'Extension Type/Property Editor UI/Collection Bulk Action Permissions',
	component: 'umb-property-editor-ui-collection-permissions',
	id: 'umb-property-editor-ui-collection-permissions',
} as Meta;

export const Docs: StoryFn<UmbPropertyEditorUICollectionPermissionsElement> = () =>
	html`<umb-property-editor-ui-collection-permissions></umb-property-editor-ui-collection-permissions>`;
