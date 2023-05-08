import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbPropertyEditorUIDropdownElement } from './property-editor-ui-dropdown.element';
import './property-editor-ui-dropdown.element';

export default {
	title: 'Property Editor UIs/Dropdown',
	component: 'umb-property-editor-ui-dropdown',
	id: 'umb-property-editor-ui-dropdown',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUIDropdownElement> = () =>
	html`<umb-property-editor-ui-dropdown></umb-property-editor-ui-dropdown>`;
AAAOverview.storyName = 'Overview';
