export const postData = async ({ url, accessToken, data }) => {
  let response = await fetch(url, {
    method: "Post",
    headers: {
      "Content-Type": "application/json",
      Accept: "application/json",
      Authorization: `Bearer ${accessToken}`,
    },
    body: JSON.stringify({ ...data }),
  });

  return response;
};

export const getData = async ({ url, accessToken }) => {
  let response = await fetch(url, {
    method: "Get",
    headers: {
      Authorization: `Bearer ${accessToken}`,
    },
  });
  return response;
};
