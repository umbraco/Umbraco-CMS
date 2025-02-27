import type { UmbPropertyEditorUIDropdownElement } from './property-editor-ui-dropdown.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-dropdown.element.js';

export default {
	title: 'Property Editor UIs/Dropdown',
	component: 'umb-property-editor-ui-dropdown',
	id: 'umb-property-editor-ui-dropdown',
} as Meta;

export const AAAOverview: StoryFn<UmbPropertyEditorUIDropdownElement> = () =>
	html`<umb-property-editor-ui-dropdown></umb-property-editor-ui-dropdown>`;
AAAOverview.storyName = 'Overview';
