import './media-workspace.element';
import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';
import { ifDefined } from 'lit/directives/if-defined.js';
import { data as mediaNodes } from '../../../../core/mocks/data/media.data';
import type { UmbMediaWorkspaceElement } from './media-workspace.element';

export default {
	title: 'Workspaces/Media',
	component: 'umb-media-workspace',
	id: 'umb-media-workspace',
} as Meta;

export const AAAOverview: Story<UmbMediaWorkspaceElement> = () =>
	html` <umb-media-workspace id="${ifDefined(mediaNodes[0].id)}"></umb-media-workspace>`;
AAAOverview.storyName = 'Overview';
