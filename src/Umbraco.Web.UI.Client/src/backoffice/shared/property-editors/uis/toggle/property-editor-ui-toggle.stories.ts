import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbPropertyEditorUIToggleElement } from './property-editor-ui-toggle.element';
import './property-editor-ui-toggle.element';

export default {
	title: 'Property Editor UIs/Toggle',
	component: 'umb-property-editor-ui-toggle',
	id: 'umb-property-editor-ui-toggle',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUIToggleElement> = () =>
	html`<umb-property-editor-ui-toggle></umb-property-editor-ui-toggle>`;
AAAOverview.storyName = 'Overview';
