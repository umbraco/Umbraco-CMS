import type { UmbPropertyEditorUITagsElement } from './property-editor-ui-tags.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-tags.element.js';

export default {
	title: 'Property Editor UIs/Tags',
	component: 'umb-property-editor-ui-tags',
	id: 'umb-property-editor-ui-tags',
} as Meta;

export const AAAOverview: StoryFn<UmbPropertyEditorUITagsElement> = () =>
	html`<umb-property-editor-ui-tags></umb-property-editor-ui-tags>`;
AAAOverview.storyName = 'Overview';
