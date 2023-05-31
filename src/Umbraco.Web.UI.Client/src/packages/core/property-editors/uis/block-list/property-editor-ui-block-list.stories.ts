import { Meta, Story } from '@storybook/web-components';
import type { UmbPropertyEditorUIBlockListElement } from './property-editor-ui-block-list.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-block-list.element.js';

export default {
	title: 'Property Editor UIs/Block List',
	component: 'umb-property-editor-ui-block-list',
	id: 'umb-property-editor-ui-block-list',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUIBlockListElement> = () =>
	html`<umb-property-editor-ui-block-list></umb-property-editor-ui-block-list>`;
AAAOverview.storyName = 'Overview';
