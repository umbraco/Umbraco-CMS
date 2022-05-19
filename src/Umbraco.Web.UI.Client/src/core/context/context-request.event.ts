export const umbContextRequestType = 'umb:context-request';

export type UmbContextCallback = (instance: any) => void;

/**
 * @export
 * @interface UmbContextRequest
 */
export interface UmbContextRequest {
  readonly contextKey: string;
  readonly callback: UmbContextCallback;
}

/**
 * @export
 * @class UmbContextRequestEvent
 * @extends {Event}
 * @implements {UmbContextRequest}
 */
export class UmbContextRequestEvent extends Event implements UmbContextRequest {
  public constructor(
    public readonly contextKey: string,
    public readonly callback: UmbContextCallback
  ) {
    super(umbContextRequestType, {bubbles: true, composed: true, cancelable: true });
  }
}

export const isUmbContextRequestEvent = (event: Event): event is UmbContextRequestEvent => {
  return event.type === umbContextRequestType;
}