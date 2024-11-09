import type { UmbPropertyEditorUITextBoxElement } from './property-editor-ui-text-box.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-text-box.element.js';

export default {
	title: 'Property Editor UIs/Text Box',
	component: 'umb-property-editor-ui-text-box',
	id: 'umb-property-editor-ui-text-box',
} as Meta;

export const AAAOverview: StoryFn<UmbPropertyEditorUITextBoxElement> = () =>
	html` <umb-property-editor-ui-text-box></umb-property-editor-ui-text-box>`;
AAAOverview.storyName = 'Overview';
