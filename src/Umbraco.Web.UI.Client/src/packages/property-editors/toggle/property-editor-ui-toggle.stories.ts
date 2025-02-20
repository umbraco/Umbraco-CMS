import type { UmbPropertyEditorUIToggleElement } from './property-editor-ui-toggle.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-toggle.element.js';

export default {
	title: 'Property Editor UIs/Toggle',
	component: 'umb-property-editor-ui-toggle',
	id: 'umb-property-editor-ui-toggle',
} as Meta;

export const AAAOverview: StoryFn<UmbPropertyEditorUIToggleElement> = () =>
	html`<umb-property-editor-ui-toggle></umb-property-editor-ui-toggle>`;
AAAOverview.storyName = 'Overview';
