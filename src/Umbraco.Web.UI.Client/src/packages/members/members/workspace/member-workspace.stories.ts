import './member-workspace.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import { data } from '../../../../mocks/data/member.data.js';

import type { UmbMemberWorkspaceElement } from './member-workspace.element.js';

export default {
	title: 'Workspaces/Member',
	component: 'umb-member-workspace',
	id: 'umb-member-workspace',
} as Meta;

export const AAAOverview: Story<UmbMemberWorkspaceElement> = () =>
	html` <umb-member-workspace id="${data[0].id}"></umb-member-workspace>`;
AAAOverview.storyName = 'Overview';
