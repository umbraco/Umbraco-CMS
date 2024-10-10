import type { UmbPropertyEditorUIBlockGridElement } from './property-editor-ui-block-grid.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-block-grid.element.js';

export default {
	title: 'Property Editor UIs/Block Grid',
	component: 'umb-property-editor-ui-block-grid',
	id: 'umb-property-editor-ui-block-grid',
} as Meta;

export const AAAOverview: StoryFn<UmbPropertyEditorUIBlockGridElement> = () =>
	html`<umb-property-editor-ui-block-grid></umb-property-editor-ui-block-grid>`;
AAAOverview.storyName = 'Overview';
