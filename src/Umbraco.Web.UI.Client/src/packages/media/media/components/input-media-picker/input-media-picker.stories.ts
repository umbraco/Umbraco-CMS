import { Meta, StoryObj } from '@storybook/web-components';
import './input-media-picker.element';
import type { UmbInputMediaPickerElement } from './input-media-picker.element';

const meta: Meta<UmbInputMediaPickerElement> = {
    title: 'Components/Inputs/Media Picker',
    component: 'umb-input-media-picker'
};
  
export default meta;
type Story = StoryObj<UmbInputMediaPickerElement>;
  
export const Overview: Story = {
    args: {
        
    }
};