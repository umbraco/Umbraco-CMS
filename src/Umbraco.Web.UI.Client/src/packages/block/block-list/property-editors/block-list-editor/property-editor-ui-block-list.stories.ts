import type { UmbPropertyEditorUIBlockListElement } from './property-editor-ui-block-list.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-block-list.element.js';

export default {
	title: 'Property Editor UIs/Block List',
	component: 'umb-property-editor-ui-block-list',
	id: 'umb-property-editor-ui-block-list',
} as Meta;

export const AAAOverview: StoryFn<UmbPropertyEditorUIBlockListElement> = () =>
	html`<umb-property-editor-ui-block-list></umb-property-editor-ui-block-list>`;
AAAOverview.storyName = 'Overview';
