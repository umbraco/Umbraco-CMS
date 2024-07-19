import type { UmbPropertyEditorUICollectionElement } from './property-editor-ui-collection.element.js';
import type { Meta, Story } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-collection.element.js';

export default {
	title: 'Property Editor UIs/Collection',
	component: 'umb-property-editor-ui-collection',
	id: 'umb-property-editor-ui-collection',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUICollectionElement> = () =>
	html`<umb-property-editor-ui-collection></umb-property-editor-ui-collection>`;
AAAOverview.storyName = 'Overview';
