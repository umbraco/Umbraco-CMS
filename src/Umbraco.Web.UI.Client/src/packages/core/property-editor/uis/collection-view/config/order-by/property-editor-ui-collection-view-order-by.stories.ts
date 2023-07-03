import { Meta, Story } from '@storybook/web-components';
import type { UmbPropertyEditorUICollectionViewOrderByElement } from './property-editor-ui-collection-view-order-by.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-collection-view-order-by.element.js';

export default {
	title: 'Property Editor UIs/Collection View Order By',
	component: 'umb-property-editor-ui-collection-view-order-by',
	id: 'umb-property-editor-ui-collection-view-order-by',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUICollectionViewOrderByElement> = () =>
	html`<umb-property-editor-ui-collection-view-order-by></umb-property-editor-ui-collection-view-order-by>`;
AAAOverview.storyName = 'Overview';
