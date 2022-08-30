import './editor-entity-layout.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbEditorEntityLayout } from './editor-entity-layout.element';

export default {
	title: 'Editors/Shared/Editor Entity Layout',
	component: 'umb-editor-entity-layout',
	id: 'umb-editor-entity-layout',
} as Meta;

export const AAAOverview: Story<UmbEditorEntityLayout> = () => html` <umb-editor-entity-layout>
	<div slot="icon"><uui-button color="" look="placeholder">Icon slot</uui-button></div>
	<div slot="name"><uui-button color="" look="placeholder">Name slot</uui-button></div>
	<div slot="footer"><uui-button color="" look="placeholder">Footer slot</uui-button></div>
	<div slot="actions"><uui-button color="" look="placeholder">Actions slot</uui-button></div>
	<uui-button color="" look="placeholder">Main slot</uui-button>
</umb-editor-entity-layout>`;
AAAOverview.storyName = 'Overview';
