import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbPropertyEditorUICollectionViewOrderByElement } from './property-editor-ui-collection-view-order-by.element';
import './property-editor-ui-collection-view-order-by.element';

export default {
	title: 'Property Editor UIs/Collection View Order By',
	component: 'umb-property-editor-ui-collection-view-order-by',
	id: 'umb-property-editor-ui-collection-view-order-by',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUICollectionViewOrderByElement> = () =>
	html`<umb-property-editor-ui-collection-view-order-by></umb-property-editor-ui-collection-view-order-by>`;
AAAOverview.storyName = 'Overview';
