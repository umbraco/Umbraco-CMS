import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbPropertyEditorUIMultipleTextStringElement } from './property-editor-ui-multiple-text-string.element';
import './property-editor-ui-multiple-text-string.element';

export default {
	title: 'Property Editor UIs/Multiple Text String',
	component: 'umb-property-editor-ui-multiple-text-string',
	id: 'umb-property-editor-ui-multiple-text-string',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUIMultipleTextStringElement> = () =>
	html`<umb-property-editor-ui-multiple-text-string></umb-property-editor-ui-multiple-text-string>`;
AAAOverview.storyName = 'Overview';
