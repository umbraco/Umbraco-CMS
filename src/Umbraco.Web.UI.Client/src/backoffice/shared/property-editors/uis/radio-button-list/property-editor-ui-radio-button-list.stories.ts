import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbPropertyEditorUIRadioButtonListElement } from './property-editor-ui-radio-button-list.element';
import './property-editor-ui-radio-button-list.element';

export default {
	title: 'Property Editor UIs/Radio Button List',
	component: 'umb-property-editor-ui-radio-button-list',
	id: 'umb-property-editor-ui-radio-button-list',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUIRadioButtonListElement> = () =>
	html`<umb-property-editor-ui-radio-button-list></umb-property-editor-ui-radio-button-list>`;
AAAOverview.storyName = 'Overview';
