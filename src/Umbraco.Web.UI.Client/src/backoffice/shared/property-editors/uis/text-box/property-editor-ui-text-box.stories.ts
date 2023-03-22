import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbPropertyEditorUITextBoxElement } from './property-editor-ui-text-box.element';
import './property-editor-ui-text-box.element';

export default {
	title: 'Property Editor UIs/Text Box',
	component: 'umb-property-editor-ui-text-box',
	id: 'umb-property-editor-ui-text-box',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUITextBoxElement> = () =>
	html` <umb-property-editor-ui-text-box></umb-property-editor-ui-text-box>`;
AAAOverview.storyName = 'Overview';
