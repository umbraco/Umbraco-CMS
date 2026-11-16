import {createServer} from "http";
import {AddressInfo} from "net";

export interface ReceivedWebhookRequest {
  method: string;
  url: string;
  content: string;
  headers: NodeJS.Dict<string[]>;
}

/**
 * Receives webhook requests in the test process. The site under test must be able to reach this machine on its
 * loopback address.
 */
export class WebhookReceiver {
  private static url: Promise<string> | undefined;
  private static readonly requests = new Map<string, ReceivedWebhookRequest[]>();

  /**
   * Starts the receiver on first use.
   *
   * @returns The receiver's base URL. A request to a path below it is stored under the first path segment.
   */
  static start(): Promise<string> {
    WebhookReceiver.url ??= new Promise((resolve, reject) => {
      const server = createServer((request, response) => {
        const chunks: Buffer[] = [];
        request.on('error', () => response.destroy());
        request.on('data', (chunk) => chunks.push(chunk));
        request.on('end', () => {
          const token = (request.url ?? '').split(/[/?]/)[1];
          const requests = WebhookReceiver.requests.get(token) ?? [];
          requests.push({
            method: request.method ?? '',
            url: request.url ?? '',
            content: Buffer.concat(chunks).toString('utf8'),
            headers: request.headersDistinct,
          });
          WebhookReceiver.requests.set(token, requests);
          response.writeHead(200).end();
        });
      });
      server.once('error', (error) => {
        // A later call gets to start the receiver again.
        WebhookReceiver.url = undefined;
        reject(error);
      });
      server.listen(0, '127.0.0.1', () => {
        // The receiver must not keep the worker process alive once the tests are done.
        server.unref();
        resolve(`http://127.0.0.1:${(server.address() as AddressInfo).port}/`);
      });
    });
    return WebhookReceiver.url;
  }

  /**
   * @returns A copy of the requests received so far for the token.
   */
  static getRequests(token: string): ReceivedWebhookRequest[] {
    return [...(WebhookReceiver.requests.get(token) ?? [])];
  }
}
