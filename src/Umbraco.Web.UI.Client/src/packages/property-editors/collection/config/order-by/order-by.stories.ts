import type { UmbPropertyEditorUICollectionOrderByElement } from './order-by.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './order-by.element.js';

export default {
	title: 'Property Editor UIs/Collection Order By',
	component: 'umb-property-editor-ui-collection-order-by',
	id: 'umb-property-editor-ui-collection-order-by',
} as Meta;

export const AAAOverview: StoryFn<UmbPropertyEditorUICollectionOrderByElement> = () =>
	html`<umb-property-editor-ui-collection-order-by></umb-property-editor-ui-collection-order-by>`;
AAAOverview.storyName = 'Overview';
