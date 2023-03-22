import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbPropertyEditorUICollectionViewBulkActionPermissionsElement } from './property-editor-ui-collection-view-bulk-action-permissions.element';
import './property-editor-ui-collection-view-bulk-action-permissions.element';

export default {
	title: 'Property Editor UIs/Collection View Bulk Action Permissions',
	component: 'umb-property-editor-ui-collection-view-bulk-action-permissions',
	id: 'umb-property-editor-ui-collection-view-bulk-action-permissions',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUICollectionViewBulkActionPermissionsElement> = () =>
	html`<umb-property-editor-ui-collection-view-bulk-action-permissions></umb-property-editor-ui-collection-view-bulk-action-permissions>`;
AAAOverview.storyName = 'Overview';
