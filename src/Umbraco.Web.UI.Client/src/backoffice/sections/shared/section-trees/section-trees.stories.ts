import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbSectionTreesElement } from './section-trees.element';
import './section-trees.element';

export default {
	title: 'Sections/Shared/Section Sidebar',
	component: 'umb-section-sidebar',
	id: 'umb-section-sidebar',
} as Meta;

export const AAAOverview: Story<UmbSectionTreesElement> = () => html` <umb-section-trees></umb-section-trees>`;
AAAOverview.storyName = 'Overview';
