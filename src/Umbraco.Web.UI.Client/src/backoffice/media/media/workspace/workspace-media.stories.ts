import './workspace-media.element';
import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';
import type { UmbWorkspaceMediaElement } from './workspace-media.element';
import { data as mediaNodes } from 'src/core/mocks/data/media.data';

export default {
	title: 'Workspaces/Media',
	component: 'umb-workspace-media',
	id: 'umb-workspace-media',
} as Meta;

export const AAAOverview: Story<UmbWorkspaceMediaElement> = () =>
	html` <umb-workspace-media id="${mediaNodes[0].key}"></umb-workspace-media>`;
AAAOverview.storyName = 'Overview';
