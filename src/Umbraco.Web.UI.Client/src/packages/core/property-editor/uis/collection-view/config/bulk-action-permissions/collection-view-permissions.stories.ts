import type { Meta, Story } from '@storybook/web-components';
import type { UmbPropertyEditorUICollectionViewPermissionsElement } from './collection-view-permissions.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './collection-view-permissions.element.js';

export default {
	title: 'Property Editor UIs/Collection View Bulk Action Permissions',
	component: 'umb-property-editor-ui-collection-view-permissions',
	id: 'umb-property-editor-ui-collection-view-permissions',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUICollectionViewPermissionsElement> = () =>
	html`<umb-property-editor-ui-collection-view-permissions></umb-property-editor-ui-collection-view-permissions>`;
AAAOverview.storyName = 'Overview';
