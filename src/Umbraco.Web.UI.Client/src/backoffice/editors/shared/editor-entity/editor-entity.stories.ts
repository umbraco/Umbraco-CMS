import { UmbEditorEntity } from './editor-entity.element';
import './editor-entity.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

export default {
	title: 'Editors/Shared/Editor Entity',
	component: 'umb-editor-entity',
	id: 'umb-editor-entity',
} as Meta;

export const AAAOverview: Story<UmbEditorEntity> = () => html` <umb-editor-entity>
	<div slot="icon"><uui-button color="" look="placeholder">Icon slot</uui-button></div>
	<div slot="name"><uui-button color="" look="placeholder">Name slot</uui-button></div>
	<div slot="footer"><uui-button color="" look="placeholder">Footer slot</uui-button></div>
	<div slot="actions"><uui-button color="" look="placeholder">Actions slot</uui-button></div>
	<uui-button color="" look="placeholder">Main slot</uui-button>
</umb-editor-entity>`;
AAAOverview.storyName = 'Overview';
