import './stylesheet-workspace.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import { data } from '../../../../core/mocks/data/member.data';

import type { UmbStylesheetWorkspaceElement } from './stylesheet-workspace.element';

export default {
	title: 'Workspaces/Member',
	component: 'umb-member-workspace',
	id: 'umb-member-workspace',
} as Meta;

export const AAAOverview: Story<UmbStylesheetWorkspaceElement> = () =>
	html` <umb-member-workspace id="${data[0].key}"></umb-member-workspace>`;
AAAOverview.storyName = 'Overview';
