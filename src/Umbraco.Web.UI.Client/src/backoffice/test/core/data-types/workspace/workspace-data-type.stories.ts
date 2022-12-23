import './workspace-data-type.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import { data } from '../../../../../core/mocks/data/data-type.data';

import type { UmbWorkspaceDataTypeElement } from './workspace-data-type.element';

export default {
	title: 'Workspaces/Data Type',
	component: 'umb-workspace-data-type',
	id: 'umb-workspace-data-type',
} as Meta;

export const AAAOverview: Story<UmbWorkspaceDataTypeElement> = () =>
	html` <umb-workspace-data-type id="${data[0].key}"></umb-workspace-data-type>`;
AAAOverview.storyName = 'Overview';
